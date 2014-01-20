using System.Text;

namespace Sokvihittar.Crawlers.Common
{
    public interface ICrawlerRequest
    {
        /// <summary>
        /// Crawler Id, used for sorting crawler results.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Source website domain.
        /// </summary>
        string Domain { get; }

        /// <summary>
        /// Needed product count.
        /// </summary>
        int Limit { get; }

        /// <summary>
        /// Search text.
        /// </summary>
        string ProductText { get; }

        /// <summary>
        /// Name of source website.
        /// </summary>
        string SourceName { get; }

        /// <summary>
        /// Encoding used on source website.
        /// </summary>
        Encoding Encoding { get; }

        bool IsStrictResults { get; }

        /// <summary>
        /// Executes search request, forms and returns product information models.
        /// </summary>
        /// <returns>Returns array of models containing information about product.</returns>
        ProductInfo[] ExecuteSearchRequest();
    }
}