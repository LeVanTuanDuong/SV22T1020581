using SV22T1020581.DataLayers.Interfaces;
using SV22T1020581.DataLayers.SQLServer;
using SV22T1020581.Models.DataDictionary;
using System.Threading.Tasks;

namespace SV22T1020581.BusinessLayers
{
    /// <summary>
    /// Cung c?p các ch?c nang x? lý d? li?u liên quan d?n t? di?n d? li?u
    /// </summary>
    public static class DictionaryDataService
    {
        private static readonly IDataDictionaryRepository<Province> provinceDB;

        /// <summary>
        /// Ctor
        /// </summary>
        static DictionaryDataService()
        {
            provinceDB = new ProvinceRepository(Configuration.ConnectionString);
        }
        /// <summary>
        /// L?y danh sách t?nh thành
        /// </summary>
        /// <returns></returns>
        public static async Task<IReadOnlyList<Province>> ListProvincesAsync()
        {
            return await provinceDB.ListAsync();
        }
    }
}
