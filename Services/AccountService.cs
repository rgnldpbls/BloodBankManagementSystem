namespace BBMS.Services
{
    public class AccountService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AccountService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? GetAccountId()
        {
            var accountIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("AccountId");
            if (accountIdClaim != null && int.TryParse(accountIdClaim.Value, out int accountId))
            {
                return accountId;
            }

            return null;
        }
    }
}
