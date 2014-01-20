using System.Linq;

namespace Sokvihittar.Crawlers.Common
{
   static class ProductInfoExtensions
   {
       public static bool IsStrict(this ProductInfo info, string searchText)
       {
           var type = typeof (ProductInfo);
           var propertyValues = type.GetProperties().Select(p => ((string) p.GetValue(info)).ToLower()).ToArray();
           foreach (var word in searchText.Split(' '))
           {
               if (propertyValues.Any(propertyValue => propertyValue.ToLower().Contains(word.ToLower())) == false)
               {
                   return false;
               }
           }
           return true;
       }
   }
}
