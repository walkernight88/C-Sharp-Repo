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
            foreach(var docField in DocumentFields)
            {
                if (classField == docField)
                {
                    return true;
                }
            }
            return false;
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
                    if (!FieldInClass<T>(DocField))
                    {
                        var delQuery = Query.Exists(DocField);
                        Collection.Update(delQuery, Update.Unset(DocField), UpdateFlags.Multi);
                    }
                }
            }

            var result = typeof(T).GetProperties().Where(p => (p.GetGetMethod() ?? p.GetSetMethod()).IsDefined(typeof(CompilerGeneratedAttribute), false)).Select(p => new { p.Name });

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
                            Collection.Update(addQuery, Update.Set(ClassField.Name, BsonNull.Value), UpdateFlags.Multi);
                        }
                    }
                }
            }
            
            return true;
        }