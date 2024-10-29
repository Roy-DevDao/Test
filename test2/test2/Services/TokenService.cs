using Microsoft.Extensions.Caching.Memory;

namespace test2.Services
{
    public class TokenService
    {
        private readonly IMemoryCache _cache;

        // Constructor nhận MemoryCache từ Dependency Injection
        public TokenService(IMemoryCache cache)
        {
            _cache = cache;
        }

        // Phương thức tạo token và lưu vào MemoryCache
        public string GenerateToken(string email)
        {
            // Tạo token duy nhất (UUID)
            var token = Guid.NewGuid().ToString();

            // Cài đặt thời gian hết hạn cho token (15 phút)
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(120));

            // Lưu token vào cache với key là token và value là email
            _cache.Set(token, email, cacheEntryOptions);

            return token;
        }

        // Phương thức xác thực token dựa trên email và token đã lưu trong MemoryCache
        public bool ValidateToken(string token, out string email)
        {
            if (_cache.TryGetValue(token, out string cachedEmail))
            {
                email = cachedEmail;
                _cache.Remove(token);  // Xóa token khỏi cache sau khi xác thực
                return true;
            }

            email = null;  // Gán null nếu token không tồn tại hoặc không hợp lệ
            return false;
        }
    }
}