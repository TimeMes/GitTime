using MongoDB.Bson;
using System;
using System.Runtime.InteropServices.JavaScript;
using static Reader;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

static class Reader
{
    public delegate bool Question<T>(T readline);
    public delegate void Backer();
    static public Backer backToMenu = () => Console.WriteLine("Backed to Menu");
    public static T ReadLine<T>()
    {
        string? s = Console.ReadLine();
        if (s is null)
        {
            Console.WriteLine("Null Error");
            return ReadLine<T>();
        }
        else if(s.ToLower() == "end")
        {
            Environment.Exit(0);
            return ReadLine<T>();
        }
        else if(s.ToLower() == "menu" && backToMenu is not null)
        {
            backToMenu();
            Environment.Exit(0);
            return default;
        }
        else
        {
            try
            {
                return (T)Convert.ChangeType(s, typeof(T));
            }
            catch
            {
                Console.WriteLine($"Not {typeof(T)}");
                return ReadLine<T>();
            }
        }
    }

    public static T ReadLine<T>(Question<T> question)
    {
        T t = ReadLine<T>();
        if (question(t)) return t;
        else
        {
            Console.WriteLine($"Invalid value");
           
            return ReadLine<T>(question);
        }
    }
}

static class ReaderPlus
{
    private static string Comparer = "$";
    public static BsonDocument CompareInt()
    {
        Comparer = "$";
        string? s = Console.ReadLine();
        int Skip = 1;
        if(s is null || s.Length <  2)
        {
            Console.WriteLine("Null Error");
            return CompareInt();
        }
        if (s[0] == '>')
        {
            Comparer += "gt";
        }
        else if (s[0] == '<')
        {
            Comparer += "lt";
        }
        else
        {
            Console.WriteLine("Invalid Value");
            return CompareInt();
        }
        if (s[1] == '=')
        {
            Comparer += "e";
            Skip = 2;
        }

        s = string.Join("", s.Skip(Skip));


        if (int.TryParse(s, out int result))
        {
            return new BsonDocument(Comparer, result);
        }
        else
        {
            Console.WriteLine("Invalid Value");
            return CompareInt();
        }
    }
}
