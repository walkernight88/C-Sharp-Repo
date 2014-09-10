static class CollectionSyncer
        {
            public static bool SyncSubClassWithDocument(Type Subclass)
            {
                FieldInfo[] mySubClassFields = Subclass.GetFields(BindingFlags.Public |
                                                  BindingFlags.NonPublic |
                                                  BindingFlags.Instance);
                foreach (var item in mySubClassFields)
                {
                    Console.WriteLine(item.Name);
                }
                return true;
            }


            public static bool FieldInClass<T>(string DocumentField)
            {
                Type myType = typeof(T);
                var result = myType.GetProperties().Where(p => (p.GetGetMethod() ?? p.GetSetMethod()).IsDefined(typeof(CompilerGeneratedAttribute), false)).Select(p => new { p.Name });
                foreach (var item in result)
                {
                    if (DocumentField == item.Name)
                    {
                        return true;
                    }
                }
                return false;
            }

            public static bool FieldInDocument(string classField, IEnumerable<string> DocumentFields)
            {
                foreach (var docField in DocumentFields)
                {
                    if (classField == docField)
                    {
                        return true;
                    }
                }
                return false;
            }

            public static object defaultValueForClass(Type classType, string FieldName)
            {
                var ClassField = classType.GetField("<" + FieldName + ">k__BackingField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (ClassField.FieldType.FullName == "System.String")
                    return string.Empty;
                return null;
            }


            public static bool SyncClassWithCollection<T>(MongoCollection Collection)
            {
                var query =
                    from e in Collection.AsQueryable<BsonDocument>()
                    select e;


                foreach (var i in query)
                {
                    IEnumerable<string> myDocFields = i.Names;
                    foreach (var DocField in myDocFields.Where(f => f.ToLowerInvariant() != idFieldFromDb))
                    {
                        IEnumerable<Type> subClassTypes = typeof(T).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(T)));

                        if (!FieldInClass<T>(DocField))
                        {
                            var delQuery = Query.Exists(DocField);
                            Collection.Update(delQuery, Update.Unset(DocField), UpdateFlags.Multi);
                        }
                    }
                }

                var result = typeof(T).GetProperties().Where(p => (p.GetGetMethod() ?? p.GetSetMethod()).IsDefined(typeof(CompilerGeneratedAttribute), false)).Select(p => new { p.Name });

                var tempClassVar = (T)Activator.CreateInstance(typeof(T), new object[] { });


                foreach (var i in query)
                {
                    IEnumerable<string> oneDocFields = i.Names;
                    foreach (var ClassField in result.Where(f => f.Name.ToLowerInvariant() != idFieldFromClass))
                    {

                        AttributeCollection attributes = TypeDescriptor.GetProperties(typeof(T))[ClassField.Name].Attributes;

                        DefaultValueAttribute myAttribute = (DefaultValueAttribute)attributes[typeof(DefaultValueAttribute)];
                        if (!FieldInDocument(ClassField.Name, oneDocFields))
                        {
                            var addQuery = Query.NotExists(ClassField.Name);
                            if (myAttribute != null)
                            {
                                Collection.Update(addQuery, Update.Set(ClassField.Name, BsonValue.Create(myAttribute.Value)), UpdateFlags.Multi);
                            }
                            else
                            {
                                if (tempClassVar.GetType().GetField("<" + ClassField.Name + ">k__BackingField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tempClassVar) == null)
                                {
                                    Collection.Update(addQuery, Update.Set(ClassField.Name, BsonValue.Create(defaultValueForClass(typeof(T), ClassField.Name))), UpdateFlags.Multi);

                                }
                                else
                                {
                                    Collection.Update(addQuery, Update.Set(ClassField.Name, BsonValue.Create(tempClassVar.GetType().GetField("<" + ClassField.Name + ">k__BackingField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tempClassVar))), UpdateFlags.Multi);
                                }
                            }
                        }
                    }
                }

                return true;
            }
        }