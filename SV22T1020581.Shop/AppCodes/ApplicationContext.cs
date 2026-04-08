using Newtonsoft.Json;

namespace SV22T1020581.Shop.AppCodes
{
    /// <summary>
    /// Lớp cung cấp các tiện ích liên quan đến ngữ cảnh của ứng dụng web
    /// </summary>
    public static class ApplicationContext
    {
        private static IHttpContextAccessor? _httpContextAccessor;
        private static IWebHostEnvironment? _webHostEnvironment;
        private static IConfiguration? _configuration;

        /// <summary>
        /// Gọi hàm này trong Program.cs để khởi tạo ngữ cảnh
        /// </summary>
        /// <param name="httpContextAccessor">IHttpContextAccessor từ service provider</param>
        /// <param name="webHostEnvironment">IWebHostEnvironment từ service provider</param>
        /// <param name="configuration">IConfiguration từ builder</param>
        /// <exception cref="ArgumentNullException">Khi tham số đầu vào là null</exception>
        public static void Configure(
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException();
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException();
            _configuration = configuration ?? throw new ArgumentNullException();
        }

        /// <summary>
        /// HttpContext hiện tại
        /// </summary>
        public static HttpContext? HttpContext => _httpContextAccessor?.HttpContext;

        /// <summary>
        /// WebHostEnvironment hiện tại
        /// </summary>
        public static IWebHostEnvironment? WebHostEnviroment => _webHostEnvironment;

        /// <summary>
        /// Configuration của ứng dụng
        /// </summary>
        public static IConfiguration? Configuration => _configuration;

        /// <summary>
        /// URL gốc của website (kết thúc bởi dấu /)
        /// </summary>
        public static string BaseURL => $"{HttpContext?.Request.Scheme}://{HttpContext?.Request.Host}/";

        /// <summary>
        /// Đường dẫn vật lý đến thư mục wwwroot
        /// </summary>
        public static string WWWRootPath => _webHostEnvironment?.WebRootPath ?? string.Empty;

        /// <summary>
        /// Đường dẫn vật lý đến thư mục gốc của ứng dụng web
        /// </summary>
        public static string ApplicationRootPath => _webHostEnvironment?.ContentRootPath ?? string.Empty;

        /// <summary>
        /// Ghi dữ liệu vào session
        /// </summary>
        /// <param name="key">Khóa của dữ liệu trong session</param>
        /// <param name="value">Giá trị cần lưu</param>
        public static void SetSessionData(string key, object value)
        {
            try
            {
                string sValue = JsonConvert.SerializeObject(value);
                if (!string.IsNullOrEmpty(sValue))
                {
                    _httpContextAccessor?.HttpContext?.Session.SetString(key, sValue);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Đọc dữ liệu từ session
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu cần đọc</typeparam>
        /// <param name="key">Khóa của dữ liệu trong session</param>
        /// <returns>Dữ liệu đã đọc hoặc null nếu không tồn tại</returns>
        public static T? GetSessionData<T>(string key) where T : class
        {
            try
            {
                string sValue = _httpContextAccessor?.HttpContext?.Session.GetString(key) ?? "";
                if (!string.IsNullOrEmpty(sValue))
                {
                    return JsonConvert.DeserializeObject<T>(sValue);
                }
            }
            catch
            {
            }
            return null;
        }

        /// <summary>
        /// Lấy giá trị cấu hình từ appsettings.json
        /// </summary>
        /// <param name="name">Tên cấu hình</param>
        /// <returns>Giá trị cấu hình hoặc chuỗi rỗng</returns>
        public static string GetConfigValue(string name)
        {
            return _configuration?[name] ?? "";
        }

        /// <summary>
        /// Lấy đối tượng cấu hình từ appsettings.json
        /// </summary>
        /// <typeparam name="T">Kiểu cấu hình</typeparam>
        /// <param name="name">Tên cấu hình</param>
        /// <returns>Đối tượng cấu hình</returns>
        public static T GetConfigSection<T>(string name) where T : new()
        {
            var value = new T();
            _configuration?.GetSection(name).Bind(value);
            return value;
        }
    }
}
