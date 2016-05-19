 static class CollectionSyncer
        {
            public static void DocumentInDocument(Type classType,BsonDocument document, string initialField,MongoCollection Collection,string queryString) // Removes fields from subdocuments which are not in classes (STRUCTS)
            {
                var SubClassField = classType.GetField("<"+initialField+">k__BackingField",BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                if (SubClassField != null)
                {
                    var SubClassNestedFields = SubClassField.FieldType.GetProperties().Where(p => (p.GetGetMethod() ?? p.GetSetMethod()).IsDefined(typeof(CompilerGeneratedAttribute), false)).Select(p => new { p.Name });
                    IEnumerable<string> myDocFields = document.Names;

                    foreach (var docField in myDocFields)
                    {

                        if (!FieldInSubClass(SubClassField.FieldType, docField))
                        {
                            if (queryString == null)
                            {
                                var delQuery = Query.EQ(initialField + "." + docField, document.GetElement(docField).Value);
                                var update = Update.Unset(initialField + "." + docField);
                                Collection.Update(delQuery, update, UpdateFlags.Multi);
                            }
                            else
                            {
                                var delQuery = Query.EQ(queryString + "." + initialField + "." + docField, document.GetElement(docField).Value);
                                var update = Update.Unset(queryString + "." + initialField + "." + docField);
                                Collection.Update(delQuery, update, UpdateFlags.Multi);
                            }

                        }
                        if (document.GetElement(docField).Value.GetType().Name == "BsonDocument") // Goes down on subdocuments tree
                        {
                            DocumentInDocument(SubClassField.FieldType, document.GetElement(docField).Value.ToBsonDocument(), docField, Collection, initialField);
                        }
                    }
                }
                else
                {
                    var delQuery = Query.Exists(initialField);
                    var update = Update.Unset(initialField);
                    Collection.Update(delQuery, update, UpdateFlags.Multi);
                }
               
            }

            public static void DocumentInDocumentArray(Type classType, BsonDocument document, string initialField, MongoCollection Collection, string queryString, int arrayIndex) // Removes fields from subdocuments which are not in classes (STRUCTS)
            {
                var SubClassField = classType.GetField("<" + initialField + ">k__BackingField", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                if (SubClassField != null)
                {
                    var SubClassNestedFields = SubClassField.FieldType.GetGenericArguments()[0].GetProperties().Where(p => (p.GetGetMethod() ?? p.GetSetMethod()).IsDefined(typeof(CompilerGeneratedAttribute), false)).Select(p => new { p.Name });
                    IEnumerable<string> myDocFields = document.Names;

                    foreach (var docField in myDocFields)
                    {

                        if (!FieldInSubClass(SubClassField.FieldType.GetGenericArguments()[0], docField))
                        {
                            if (queryString == null)
                            {
                                var delQuery = Query.EQ(initialField + "." + arrayIndex.ToString() + "." + docField, document.GetElement(docField).Value);
                                var update = Update.Unset(initialField + "." + arrayIndex.ToString() + "." + docField);
                                Collection.Update(delQuery, update, UpdateFlags.Multi);
                            }
                            else
                            {
                                var delQuery = Query.EQ(queryString + "." + arrayIndex.ToString() + "." + initialField + "." + docField, document.GetElement(docField).Value);
                                var update = Update.Unset(queryString + "." + arrayIndex.ToString() + "." + initialField + "." + docField);
                                Collection.Update(delQuery, update, UpdateFlags.Multi);
                            }

                        }
                        if (document.GetElement(docField).Value.GetType().Name == "BsonDocument") // Goes down on subdocuments tree
                        {
                            DocumentInDocumentArray(SubClassField.FieldType.GetGenericArguments()[0], document.GetElement(docField).Value.ToBsonDocument(), docField, Collection, initialField, arrayIndex);
                        }
                    }
                }
                else
                {
                    var delQuery = Query.Exists(initialField + "." + arrayIndex.ToString());
                    var update = Update.Unset(initialField + "." + arrayIndex.ToString());
                    Collection.Update(delQuery, update, UpdateFlags.Multi);
                }

            }

            public static void AddFieldsInSubdocuments(Type classType, BsonDocument document, string initialField, MongoCollection Collection, string queryString) // Add fields to subdocuments which are in classes (STRUCTS)
            {
                var SubClassField = classType.GetField("<" + initialField + ">k__BackingField", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                if (SubClassField != null)
                {
                    var SubClassNestedFields = SubClassField.FieldType.GetProperties().Where(p => (p.GetGetMethod() ?? p.GetSetMethod()).IsDefined(typeof(CompilerGeneratedAttribute), false)).Select(p => new { p.Name });
                    IEnumerable<string> myDocFields = document.Names;

                    foreach (var subClassField in SubClassNestedFields)
                    {
                        AttributeCollection attributes = TypeDescriptor.GetProperties(SubClassField.FieldType)[subClassField.Name].Attributes;
                        DefaultValueAttribute myAttribute = (DefaultValueAttribute)attributes[typeof(DefaultValueAttribute)];
                        bool isDocument = false;
                        
                        if (!FieldInDocument(subClassField.Name, myDocFields))
                        {
                            
                            IMongoQuery addQuery;
                            if (queryString == null)
                            {
                                addQuery = Query.NotExists(initialField + "." + subClassField.Name);
                                
                            

                            if (myAttribute != null)
                            {
                                Collection.Update(addQuery, Update.Set(initialField+"."+subClassField.Name, BsonValue.Create(myAttribute.Value)), UpdateFlags.Multi);
                            }
                            else
                            {
                                var tempObj = Activator.CreateInstance(SubClassField.FieldType, new object[] { });
                                if (SubClassField.FieldType.GetField("<" + subClassField.Name + ">k__BackingField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tempObj) == null)
                                {
                                    Collection.Update(addQuery, Update.Set(initialField+"."+subClassField.Name, BsonValue.Create(defaultValueForClass(SubClassField.FieldType, subClassField.Name))), UpdateFlags.Multi);

                                }
                                else
                                {
                                    BsonDocument newDoc = new BsonDocument{};
                                    Collection.Update(addQuery, Update.Set(initialField+"."+subClassField.Name, newDoc), UpdateFlags.Multi);
                                    isDocument = true;
                                }


                            }
                            }
                            else
                            {
                                addQuery = Query.NotExists(queryString + "." + initialField + "." + subClassField.Name);
                                if (myAttribute != null)
                                {
                                    Collection.Update(addQuery, Update.Set(queryString + "." + initialField + "." + subClassField.Name, BsonValue.Create(myAttribute.Value)), UpdateFlags.Multi);
                                }
                                else
                                {
                                    var tempObj = Activator.CreateInstance(SubClassField.FieldType, new object[] { });
                                    if (SubClassField.FieldType.GetField("<" + subClassField.Name + ">k__BackingField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tempObj) == null)
                                    {
                                        Collection.Update(addQuery, Update.Set(queryString + "." + initialField + "." + subClassField.Name, BsonValue.Create(defaultValueForClass(SubClassField.FieldType, subClassField.Name))), UpdateFlags.Multi);

                                    }
                                    else
                                    {
                                        BsonDocument newDoc = new BsonDocument{};
                                        Collection.Update(addQuery, Update.Set(queryString + "." + initialField + "." + subClassField.Name, newDoc), UpdateFlags.Multi);
                                        isDocument = true;
                                    }


                                }
                            }
                        }
                        if (isDocument) // Goes down on subdocuments tree
                        {
                            AddFieldsInSubdocuments(SubClassField.FieldType, document, subClassField.Name, Collection, initialField);
                        }


                    }
                }
                    
            }

            public static void AddFieldsInSubdocumentsArray(Type classType, BsonDocument document, string initialField, MongoCollection Collection, string queryString, int arrayIndex) // Add fields to subdocuments which are in classes (STRUCTS)
            {
                var SubClassField = classType.GetField("<" + initialField + ">k__BackingField", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                if (SubClassField != null)
                {
                    var SubClassNestedFields = SubClassField.FieldType.GetGenericArguments()[0].GetProperties().Where(p => (p.GetGetMethod() ?? p.GetSetMethod()).IsDefined(typeof(CompilerGeneratedAttribute), false)).Select(p => new { p.Name });
                    IEnumerable<string> myDocFields = document.Names;

                    foreach (var subClassField in SubClassNestedFields)
                    {
                        AttributeCollection attributes = TypeDescriptor.GetProperties(SubClassField.FieldType.GetGenericArguments()[0])[subClassField.Name].Attributes;
                        DefaultValueAttribute myAttribute = (DefaultValueAttribute)attributes[typeof(DefaultValueAttribute)];
                        bool isDocument = false;

                        if (!FieldInDocument(subClassField.Name, myDocFields))
                        {

                            IMongoQuery addQuery;
                            if (queryString == null)
                            {
                                addQuery = Query.NotExists(initialField +"."+arrayIndex.ToString()+ "." + subClassField.Name);



                               if (myAttribute != null)
                                {
                                    Collection.Update(addQuery, Update.Set(initialField + "." + arrayIndex.ToString() + "." + subClassField.Name, BsonValue.Create(myAttribute.Value)), UpdateFlags.Multi);
                                }
                                else
                                {
                                    var tempObj = Activator.CreateInstance(SubClassField.FieldType.GetGenericArguments()[0], new object[] { });
                                    if (SubClassField.FieldType.GetGenericArguments()[0].GetField("<" + subClassField.Name + ">k__BackingField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tempObj) == null)
                                    {
                                        Collection.Update(addQuery, Update.Set(initialField + "." + arrayIndex.ToString() + "." + subClassField.Name, BsonValue.Create(defaultValueForClass(SubClassField.FieldType.GetGenericArguments()[0], subClassField.Name))), UpdateFlags.Multi);

                                    }
                                    else
                                    {
                                        BsonDocument newDoc = new BsonDocument { };
                                        Collection.Update(addQuery, Update.Set(initialField + "." + arrayIndex.ToString() + "." + subClassField.Name, newDoc), UpdateFlags.Multi);
                                        isDocument = true;
                                    }


                                }
                            }
                            else
                            {
                                addQuery = Query.NotExists(queryString + "." + initialField + "." + subClassField.Name);
                                if (myAttribute != null)
                                {
                                    Collection.Update(addQuery, Update.Set(queryString + "." + initialField + "." + arrayIndex.ToString() + "." + subClassField.Name, BsonValue.Create(myAttribute.Value)), UpdateFlags.Multi);
                                }
                                else
                                {
                                    var tempObj = Activator.CreateInstance(SubClassField.FieldType.GetGenericArguments()[0], new object[] { });
                                    if (SubClassField.FieldType.GetGenericArguments()[0].GetField("<" + subClassField.Name + ">k__BackingField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tempObj) == null)
                                    {
                                        Collection.Update(addQuery, Update.Set(queryString + "." + initialField + "." + arrayIndex.ToString() + "." + subClassField.Name, BsonValue.Create(defaultValueForClass(SubClassField.FieldType.GetGenericArguments()[0], subClassField.Name))), UpdateFlags.Multi);

                                    }
                                    else
                                    {
                                        BsonDocument newDoc = new BsonDocument { };
                                        Collection.Update(addQuery, Update.Set(queryString + "." + initialField + "." + arrayIndex.ToString() + "." + subClassField.Name, newDoc), UpdateFlags.Multi);
                                        isDocument = true;
                                    }


                                }
                            }
                        }
                        if (isDocument) // Goes down on subdocuments tree
                        {
                            AddFieldsInSubdocumentsArray(SubClassField.FieldType.GetGenericArguments()[0], document, subClassField.Name, Collection, initialField, arrayIndex);
                        }


                    }
                }

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

            public static bool FieldInSubClass(Type Subtype,string DocumentField)
            {

                var result = Subtype.GetProperties().Where(p => (p.GetGetMethod() ?? p.GetSetMethod()).IsDefined(typeof(CompilerGeneratedAttribute), false)).Select(p => new { p.Name });
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
                if (ClassField.FieldType.FullName == "System.Int32")
                    return 0;
                if (ClassField.FieldType.FullName.Contains("List"))
                {
                    if (ClassField.FieldType.GetGenericArguments()[0].FullName == "System.Int32")
                        return 0;
                }
                
                return null;
            }


            public static bool SyncClassWithCollection<T>(MongoCollection Collection)
            {
                var query =
                    from e in Collection.AsQueryable<BsonDocument>()
                    select e;

               
                foreach (var i in query)
                {
                    var t = i.BsonType;

                    IEnumerable<string> myDocFields = i.Names;
                    foreach (var DocField in myDocFields.Where(f => f.ToLowerInvariant() != idFieldFromDb))
                    {

                        if (!FieldInClass<T>(DocField))
                        {
                            var delQuery = Query.Exists(DocField); 
                            Collection.Update(delQuery, Update.Unset(DocField), UpdateFlags.Multi);
                        }
                        if (i.GetElement(DocField).Value.GetType().Name == "BsonDocument") // Method used for structs if ever needed :D
                        {
                            DocumentInDocument(typeof(T), i.GetElement(DocField).Value.ToBsonDocument(), DocField, Collection, null);
                        }
                        if (i.GetElement(DocField).Value.GetType().Name == "BsonArray")
                        {
                            var Array = i.GetElement(DocField).Value.AsBsonArray.ToList();
                         
                            int arrayIndex = 0;
                            foreach (var item in Array)
                            {   
                                if (item.AsBsonValue.BsonType.ToString() == "Document")
                                {
                                    DocumentInDocumentArray(typeof(T), item.AsBsonDocument, DocField, Collection, null, arrayIndex);
                                    arrayIndex++;
                                }
                               
                            }
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
                        BsonDocument newDocument = new BsonDocument();

                        if (!FieldInDocument(ClassField.Name, oneDocFields))
                        {
                            var addQuery = Query.NotExists(ClassField.Name);
                            if (tempClassVar.GetType().GetField("<" + ClassField.Name + ">k__BackingField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FieldType.Name.Contains("List") == true)
                            {
                                if (myAttribute != null)
                                {
                                    Collection.Update(addQuery, Update.Set(ClassField.Name, new BsonArray { }), UpdateFlags.Multi);
                                }
                                else
                                {
                                    if (tempClassVar.GetType().GetField("<" + ClassField.Name + ">k__BackingField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tempClassVar) == null)
                                    {
                                        Collection.Update(addQuery, Update.Set(ClassField.Name, new BsonArray { }), UpdateFlags.Multi);

                                    }
                                    else
                                    {
                                        newDocument = tempClassVar.GetType().GetField("<" + ClassField.Name + ">k__BackingField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tempClassVar).ToBsonDocument();
                                        Collection.Update(addQuery, Update.Set(ClassField.Name, newDocument), UpdateFlags.Multi);
                                    }
                                }
                            }
                            else
                            {
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
                                        newDocument = tempClassVar.GetType().GetField("<" + ClassField.Name + ">k__BackingField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(tempClassVar).ToBsonDocument();
                                        Collection.Update(addQuery, Update.Set(ClassField.Name, newDocument), UpdateFlags.Multi);
                                    }
                                }
                            }
                        }
                    }
                }
                    var queryAfterEditing =
                   from e in Collection.AsQueryable<BsonDocument>()
                   select e;
                    foreach (var i in queryAfterEditing)
                    {
                        foreach (var ClassField in result.Where(f => f.Name.ToLowerInvariant() != idFieldFromClass))
                        {
                        var nestedTypes = typeof(T).GetNestedTypes();
                        foreach (var nestedType in nestedTypes)
                        {

                            if (nestedType.Name == tempClassVar.GetType().GetField("<" + ClassField.Name + ">k__BackingField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FieldType.Name && tempClassVar.GetType().GetField("<" + ClassField.Name + ">k__BackingField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FieldType.Name.Contains("List") == false)
                            {
                                AddFieldsInSubdocuments(typeof(T), i.GetElement(ClassField.Name).ToBsonDocument(), ClassField.Name, Collection, null);
                            }

                            if (tempClassVar.GetType().GetField("<" + ClassField.Name + ">k__BackingField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FieldType.Name.Contains("List") == true)
                            {
                                if (i.GetElement(ClassField.Name).Value.GetType().Name == "BsonArray")
                                {
                                    var Array = i.GetElement(ClassField.Name).Value.AsBsonArray.ToList();
                                    
                                    int arrayIndex = 0;
                                    foreach (var item in Array)
                                    {
                                        if (item.AsBsonValue.BsonType.ToString() == "Document")
                                        {
                                            AddFieldsInSubdocumentsArray(typeof(T), i.GetElement(ClassField.Name).ToBsonDocument(), ClassField.Name, Collection, null, arrayIndex);
                                            arrayIndex++;
                                        }
                                       
                                    }
                                }
                           
                            }
                        }


                    }
                }

                return true;
            }
        }
