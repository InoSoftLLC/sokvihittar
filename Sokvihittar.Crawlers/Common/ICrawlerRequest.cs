using System.Text;

namespace Sokvihittar.Crawlers.Common
{
    public interface ICrawlerRequest
    {
        int Id { get; }

        string Domain { get; }

        int Limit { get; }

        string ProductText { get; }

        string SourceName { get; }

        Encoding Encoding { get; }

        ProductInfo[] ProceedSearchRequest();
    }
}