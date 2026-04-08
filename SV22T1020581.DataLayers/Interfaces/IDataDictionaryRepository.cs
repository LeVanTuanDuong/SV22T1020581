namespace SV22T1020581.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các phép xử lý dữ liệu dùng cho từ điển dữ liệu (Province, ...).
    /// </summary>
    /// <typeparam name="T">Entity cần truy vấn.</typeparam>
    public interface IDataDictionaryRepository<T> where T : class
    {
        /// <summary>Lấy danh sách tất cả bản ghi.</summary>
        Task<IReadOnlyList<T>> ListAsync();
    }
}
