using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Linq.Expressions;

namespace MongoTest
{
   
    class Program
    {
        private static readonly string idFieldFromDb = "_id";
        private static readonly string idFieldFromClass = "<id>k__backingfield";

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


        public static bool SyncClassWithCollection<T>(MongoCollection Collection)
        {
            var query = 
                from e in Collection.AsQueryable<BsonDocument>()
                select e;

            Type tempClass = typeof(T);
            FieldInfo[] myClassFields = tempClass.GetFields(BindingFlags.Public | 
                                              BindingFlags.NonPublic | 
                                              BindingFlags.Instance);
            foreach (var i in query)
            {
                IEnumerable<string> myDocFields = i.Names;
                foreach (var DocField in myDocFields.Where(f => f.ToLowerInvariant() != idFieldFromDb))
                {   
                    bool exists = false;
                    foreach (var ClassField in myClassFields)
                    {
                        string nameClassField = ClassField.Name.Remove(0, 1);
                        nameClassField=Regex.Replace(nameClassField, ">k__BackingField", string.Empty);
                        if (nameClassField == DocField)
                        {
                            exists = true;
                        }
                        
                    }
                    if (!exists)
                    {
                        var delQuery = Query.Exists(DocField);
                        Collection.Update(delQuery, Update.Unset(DocField), UpdateFlags.Multi);
                    }
                }
            }

            foreach (var i in query)
            {
                IEnumerable<string> oneDocFields = i.Names;
                foreach (var ClassField in myClassFields.Where(f => f.Name.ToLowerInvariant() != idFieldFromClass))
                {
                    string nameClassField = ClassField.Name.Remove(0, 1);
                    nameClassField = Regex.Replace(nameClassField, ">k__BackingField", string.Empty);
                    bool exists = false;
                    foreach (var oneDocField in oneDocFields)
                    {
                        if (oneDocField == nameClassField)
                        {
                            exists = true;
                        }
                    }

                    AttributeCollection attributes = TypeDescriptor.GetProperties(tempClass)[nameClassField].Attributes;

                    DefaultValueAttribute myAttribute = (DefaultValueAttribute)attributes[typeof(DefaultValueAttribute)];
                    if (!exists)
                    {
                        var addQuery = Query.NotExists(nameClassField);
                        if (myAttribute != null)
                        {
                            Collection.Update(addQuery, Update.Set(nameClassField, BsonValue.Create(myAttribute.Value)), UpdateFlags.Multi);
                        }
                        else
                        {
                            Collection.Update(addQuery, Update.Set(nameClassField, BsonNull.Value), UpdateFlags.Multi);
                        }
                    }
                }
            }
            
            return true;
        }

        static void Main(string[] args)
        {   
            var connectionString = "mongodb://localhost";
            var client = new MongoClient(connectionString);
            var server = client.GetServer();
            var database = server.GetDatabase("test");
            var collection = database.GetCollection<Test>("test");
            
           
            /*
            var test = new Test()
            {
                FirstName = "Said",
               LastName = "Hasanein",
               
             
            };
          */
            //collection.Insert(test);

           // var id = new ObjectId("54096c85f525bd139453c9ee");
           // var query = Query.Exists("Age");
           // var query = Query.EQ("FirstName", "Said");
            //collection.Update(query, Update.Set("Age", BsonNull.Value), UpdateFlags.Multi);
            
           // Console.WriteLine("Id = " + test.Id);
           // var item = collection.FindOne(query);
           // Console.WriteLine(item.Id);
            
            if (SyncClassWithCollection<Test>(collection))
            {
                Console.WriteLine("Collection synced");
            }
             
            //collection.Insert(test);
            Console.ReadKey();
            
        }
        /*
        private static void AppSetup()
        {
            var collections = GetAllCollections();

            foreach (var item in collections)
            {
                var docs = item.AsQueryable<Test>().Where(t => t.Id != ObjectId.Empty);

                foreach (var d in docs)
                {
                    if (d.NeedUpgrade) { }
                       
                }
            }
        }
       
        private static IEnumerable<MongoCollection<dynamic>> GetAllCollections()
        {
            throw new NotImplementedException();
        }
         */
        
        [BsonIgnoreExtraElements]
        public class Test
        {
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set; }

            public string FirstName { get; set; }
            public string LastName { get; set; }

            public string PhoneNumber { get; set; }
            public int Index { get; set; }

            [DefaultValue(100)]
            public int Flo { get; set; }
            public int Said { get; set; }

            public struct SampleStruct
            {
                public string structData { get; set; }
                public string structData2 { get; set; }
            }
            public SampleStruct myStruct { get; set; }

            public class SubTest:Test
            {
                public string data1 {get; set;}
                public string data2 {get; set;}
            }

            public SubTest myClass { get; set; }
        }
        /*
        [BsonIgnoreExtraElements]
        public class Test : IMigrable<Test>
        {
            public ObjectId Id { get; set; }

            public string FirstName { get; set; }
            public string LastName { get; set; }

            public string Age { get; set; }
            public string Address { get; set; }

            #region IMigrable

            public string Version { get; set; }

            public bool NeedUpgrade { get; set; }

            public bool Upgrade(Test doc)
            {
                return true;
            }

            public bool Downgrade(Test doc)
            {
                throw new NotImplementedException();
            }

            public bool Create(Test doc)
            {
                throw new NotImplementedException();
            }

            #endregion

        }
         */
    }
}
