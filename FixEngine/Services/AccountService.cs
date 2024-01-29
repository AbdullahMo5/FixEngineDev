using FixEngine.Data;

namespace Services
{
    public class AccountService
    {
        private ILogger<AccountService> _logger;
        private DatabaseContext _databaseContext;

        public AccountService(ILogger<AccountService> logger, DatabaseContext databaseContext)
        {
            _logger = logger;
            _databaseContext = databaseContext;
        }

        public void FetchAccounts() { }
        public void FetchAccount() { }
        public void CreateAccount() { }
        public void DeleteAccount() { }
        public void UpdateAccount() { }
        public void ChangePassword() { }
    }
}
