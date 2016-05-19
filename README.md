# C-Sharp-Repo
C# Class to MongoCollection syncronizer
    
Used in C# applications to uniform a collection from MongoDB. It's useful when you want collection objects to be similar as structure. Empty fields on old objects are filled with default values. (eg. System.Int32, System.Bool)

--USAGE--

    static method SyncClassWithCollection<T>(MongoCollection collection)
    returns true if successfully translated C# class to mongoDb collection
    
    Parameters:
        <T> -> C# class
        MongoCollection -> target collection


Written in C# under Visual Studio 2010.

Created by Said Hasanein. 
You may alter, reproduce or extend the code as needed.
