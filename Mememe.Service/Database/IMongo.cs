using System.Threading.Tasks;

using Mememe.NineGag.Models;

namespace Mememe.Service.Database
{
    public interface IMongo
    {
        Task UploadArticle(Article article);
    }
}