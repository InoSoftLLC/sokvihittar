using System;
using System.IO;
using System.Reflection;
using System.Text;
using Sokvihittar.Crawlers;

namespace Sokvihittar.Console.Test
{
    internal class Program
    {
        private static void Main()
        {
            while (true)
            {
                var productText = System.Console.ReadLine();
                var limit = 20;
                var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                int j = 0;
                var result = Crawler.Search(productText, limit);
                foreach (var crawlerResult in result)
                {
                    var text = new StringBuilder();
                    text.Append(crawlerResult.Name).Append(Environment.NewLine);
                    text.Append(crawlerResult.State).Append(Environment.NewLine);
                    text.Append(crawlerResult.Count).Append(Environment.NewLine);
                    text.Append(crawlerResult.ExecutionTime).Append(Environment.NewLine);
                    int i = 0;
                    foreach (var product in crawlerResult.Products)
                    {

                        text.Append(i).Append(Environment.NewLine);
                        text.Append("Id: ").Append(product.Id).Append(Environment.NewLine);
                        text.Append("Name: ").Append(product.Name).Append(Environment.NewLine);
                        text.Append("Product url: ").Append(product.ProductUrl).Append(Environment.NewLine);
                        text.Append("Image url: ").Append(product.ImageUrl).Append(Environment.NewLine);
                        text.Append("Price: ").Append(product.Price).Append(Environment.NewLine);
                        text.Append("Date: ").Append(product.Date).Append(Environment.NewLine);
                        text.Append("Domain: ").Append(product.Domain).Append(Environment.NewLine);
                        text.Append("location: ").Append(product.Location).Append(Environment.NewLine);
                        text.Append("_______________________________________________________________________________________________________________________________________________")
                            .Append(Environment.NewLine).Append(Environment.NewLine);
                        i++;
                    }
                    var filename = Path.Combine(filePath, String.Format("{0}.txt", j));
                    if (File.Exists(filename))
                        File.Delete(filename);
                    File.WriteAllText(filename,text.ToString());
                    j++;
                }
                System.Console.WriteLine("success");
            }
           
        }
    }
}
