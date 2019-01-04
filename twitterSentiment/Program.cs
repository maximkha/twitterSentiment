using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using NaiveBayes;

namespace twitterSentiment
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            //writeToCSV(4, 4, "/Users/maxkhanov/Projects/twitterSentiment/twitterSentiment/bin/Debug/test.csv");

            Console.WriteLine("[Main/Thread]: Hello World!");

            string inputText = File.ReadAllText("/Users/maxkhanov/Projects/twitterSentiment/twitterSentiment/bin/Debug/in.csv");
            List<string> positiveSet = new List<string>();
            List<string> negativeSet = new List<string>();

            Console.WriteLine("[Main/Thread]: Started Parsing");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string[] it = inputText.Split('\n');
            Console.WriteLine(it.Length);
            for (int i = 1; i < it.Length; i++)
            {
                string entry = it[i];
                if (entry == "") continue;
                string[] columns = entry.Split(',');
                if (columns[1] == "0")
                {
                    negativeSet.Add(nbModel.trainers.clean(columns[3].ToLower(), new char[] { ' ' }));
                }
                else if (columns[1] == "1")
                {
                    positiveSet.Add(nbModel.trainers.clean(columns[3].ToLower(), new char[] { ' ' }));
                }
                else
                {
                    Console.WriteLine("[Main/Thread]:  Unknown sentiment '" + columns[1] + "'");
                }
            }

            stopwatch.Stop();
            Console.WriteLine("[Main/Thread]: Done");
            Console.WriteLine("[Main/Thread]: Time elapsed: {0}", stopwatch.Elapsed);
            //END PARSER

            Console.WriteLine("[Main/Thread]: Constructing model");
            Dictionary<string, List<string>> trainData = new Dictionary<string, List<string>>();
            trainData.Add("positive", positiveSet);
            trainData.Add("negative", negativeSet);
            nbModel.model textModel;
            textModel = nbModel.trainers.textToModel(trainData, ' ');
            Console.WriteLine("[Main/Thread]: Done");

            Console.WriteLine("[Main/Thread]: Training model");
            nbModel.trainers.textTrainDataProvider tdp = new nbModel.trainers.textTrainDataProvider();
            tdp.regCatagory("positive", positiveSet, ' ');
            tdp.regCatagory("negative", negativeSet, ' ');
            double score = nbModel.trainers.testTextModel(textModel, tdp);
            textModel = nbModel.trainers.trainTextToGoal(textModel, 75, tdp, false, false);

            Console.WriteLine("[Main/Thread]: Model with {0} catagories", textModel.catagories.Count);
            Console.WriteLine("[Main/Thread]: starting Twitter API");

            int negativePosts = 0;
            int positivePosts = 0;
            int resetCount = 3;
            int curReset = 0;
            while (true)
            {
                List<twitterClient.twitterPost> posts = twitterClient.twitterNewsHashtag("donaldtrump");
                foreach (twitterClient.twitterPost post in posts)
                {
                    List<object> parts = nbModel.trainers.clean(post.text.ToLower(), new char[] { ' ' }).Split(' ').ToList<object>();
                    Dictionary<object, double> pred = textModel.predict(parts, true);
                    string prediction = (string)pred.Keys.ElementAt(nbModel.trainers.max(pred));
                    Console.WriteLine("[Main/Thread]: USER: " + post.uid);
                    //Console.WriteLine(post.text);
                    if (!nbModel.trainers.same(pred.Values.ToList()))
                    {
                        Console.WriteLine("[Main/Thread]: Prediction: " + prediction);
                        if (prediction == "negative") negativePosts++;
                        else if (prediction == "positive") positivePosts++;
                        else Console.WriteLine("[Main/Thread]: Unknown prediction catagory");
                    }
                    else Console.WriteLine("[Main/Thread]: Prediction: None");
                }
                ratio(positivePosts, negativePosts);
                if (curReset >= resetCount) 
                { 
                    Console.WriteLine("[Main/Thread]: Writing file");
                    writeToCSV(positivePosts, negativePosts, "/Users/maxkhanov/Projects/twitterSentiment/twitterSentiment/bin/Debug/data.csv");
                    Console.WriteLine("[Main/Thread]: Done");
                    negativePosts = 0; 
                    positivePosts = 0; 
                    curReset = 0; 
                    Console.WriteLine("[Main/Thread]: Stat reset");
                }
                if (posts.Count == 0) 
                { 
                    Console.WriteLine("[Main/Thread]: No posts");
                } 
                else 
                {
                    curReset++;
                }
                System.Threading.Thread.Sleep(60000);
            }
        }

        public static void ratio(int pos, int neg)
        {
            if ((pos == 0) || (neg == 0)) 
            {
                Console.WriteLine("[Main/Thread]: {0}:{1} (Positive, Negative) Ratio", pos, neg);
                return;
            }

            int divisor = gcd(pos, neg);

            int reducedPos = division(pos, divisor);
            int reducedNeg = division(neg, divisor);
            Console.WriteLine("[Main/Thread]: {0}:{1} (Positive, Negative) Ratio", reducedPos, reducedNeg);
        }

        public static int gcd(int a, int b)
        {
            //find the gcd using the Euclid’s algorithm
            while (a != b)
                if (a < b) b = b - a;
                else a = a - b;
            //since at this point a=b, the gcd can be either of them
            //it is necessary to pass the gcd to the main function
            return (a);
        }

        public static int division(int a, int b)
        {
            int remainder = a, quotient = 0;
            while (remainder >= b)
            {
                remainder = remainder - b;
                quotient++;
            }
            return (quotient);
        }

        public static void writeToCSV(int a, int b, string s)
        {
            bool writeHeader = false;
            if (!File.Exists(s))
            {
                writeHeader = true;
                FileStream fs = File.Create(s);
                fs.Close();
            }
            if(writeHeader) File.AppendAllText(s, "dateTimeRecorded,twitterSentiment" + Environment.NewLine);
            File.AppendAllText(s, DateTime.Now.ToString("MM/dd/yyyy hh:mm tt") + "," + (((double)a / (a + b))).ToString("0." + new string('#', 339)) + Environment.NewLine);
        }
    }
}