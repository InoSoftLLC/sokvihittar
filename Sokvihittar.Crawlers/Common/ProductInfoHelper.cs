using System.Linq;

namespace Sokvihittar.Crawlers.Common
{
   static class ProductInfoExtensions
   {
       public static bool IsStrict(this ProductInfo info, string searchText)
       {
           return info.IsStrict(searchText.Split(' '));
       }

       public static bool IsStrict(this ProductInfo info, string[] searchWords)
       {
           var type = typeof(ProductInfo);
           var propertyValues = type.GetProperties().Select(p => ((string)p.GetValue(info)).ToLower()).ToArray();
           return searchWords.All(word => propertyValues.Any(propertyValue => propertyValue.ToLower().Contains(word.ToLower())));
       }
   }
}
