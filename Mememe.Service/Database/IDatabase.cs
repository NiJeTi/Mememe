using System.Threading.Tasks;

using Mememe.NineGag.Models;

namespace Mememe.Service.Database
{
    public interface IDatabase
    {
        Task<bool> UploadArticle(Article article);
    }
}