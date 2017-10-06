using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core;
using MongoDB.Driver.Linq;
using System.Linq.Expressions;


namespace MongoTest
{
    class Program
    {
        static void Main(string[] args)
        {
            MongoContext _context = new MongoContext();
            IMongoCollection<Record> _records = _context.Records;
            IMongoCollection<BsonDocument> _bsonRecords = _context.BsonRecords;

            //Inserting to mongo with schema
            Record _record = new Record() { name = "Aaleahya", age = 3 };
            _records.InsertOne(_record);

            //Updating a record
            var filter = Builders<Record>.Filter.Where(d => d.name.ToLowerInvariant().Contains("j"));
            var update = Builders<Record>.Update.Set(d => d.age, 100);
            var res = _records.UpdateOne(filter, update);

            
            //Insert Many without schema
            var document = new BsonDocument[]
                            {
                                new BsonDocument
                                {   {"name","Someone" },
                                    {"age",600 }
                                },
                                new BsonDocument
                                {   {"name","IncludingArray" },
                                    {"age",601 },
                                    {"array",new BsonArray{"blue","white","black"} }
                                },
                                new BsonDocument
                                {   {"name","IncludingDocument" },
                                    {"age",602 },
                                    {"Document",new BsonDocument{{"Attribute1","test" },{ "Attribute2", "test2" },{"Attribute3","test3"} } }
                                }
                            };

            _bsonRecords.InsertMany(document);

            //Testing simple filter
            var empFilter = Builders<BsonDocument>.Filter.Empty;
            IList<BsonDocument> lstDoc = _bsonRecords.Find<BsonDocument>(empFilter).ToList<BsonDocument>();

            foreach (BsonDocument doc in lstDoc)
            {
                foreach (BsonElement elm in doc)
                    Console.Write(elm.Value);
                Console.WriteLine();
            }

            //Or filter
            var builder = Builders<BsonDocument>.Filter;
            var orFilter = builder.Or(builder.Eq("name", "Someone"), builder.Eq("name", "IncludingArray"));
            var result = _bsonRecords.Find(orFilter).ToList();

            //Search on array
            var arrayFilter = builder.Eq("array", new[] { "blue", "white","black" }); // Looks for exact set
            result = _bsonRecords.Find(arrayFilter).ToList();
            arrayFilter = builder.All("array", new[] { "blue", "white" });// All items which array elements as blue, white
            result = _bsonRecords.Find(arrayFilter).ToList();

            //Size Filter with projection
            var sizeFilter = builder.Size("array", 3);
            var project = Builders<BsonDocument>.Projection.Exclude("_id");
            result = _bsonRecords.Find(sizeFilter).Project(project).ToList();
            result = _bsonRecords.Find(sizeFilter).ToList();


            var multiMatch = builder.ElemMatch<BsonValue>("array", new BsonDocument { { "$eq", "blue" }, { "$eq", "white" } }); //Multiple conditions
            result = _bsonRecords.Find(multiMatch).ToList();

            Console.Read();
          
            
        }
    }

    class MongoContext
    {
        IMongoDatabase db;

        public MongoContext()
        {
            IMongoClient client = new MongoClient();
            db = client.GetDatabase("TestCSharp");
        }

        // Enforcing schema for the collection through
        public IMongoCollection<Record> Records
        {
            get {
                    return db.GetCollection<Record>("Records");
                }
        }

        
        public IMongoCollection<BsonDocument> BsonRecords
        {
            get
            {
                return db.GetCollection<BsonDocument>("Records");
            }
        }


    }

    //Schema for Record collection
    class Record
    {
        public ObjectId _id { get; set; }
        public string name { get; set; }
        public int age { get; set; }
    }
}
