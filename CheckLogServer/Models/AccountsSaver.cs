namespace CheckLogServer.Models
{
    public class AccountsSaver : ArraySaver<Account>
    {
        public AccountsSaver() : base(@"Data\Accounts.json") { }
    }
}
