# Tapas.cs
Mobile-API for Tapas your home for the world's most exciting and diverse web comics and novels

## Example
```cs
using TapasApi;

namespace Application
{
    internal class Program
    {
        static async Task Main()
        {
            var api = new Tapas();
            await api.Login("example@gmail.com", "password");
            string accountCoins = await api.GetAccountCoins();
            Console.WriteLine(accountCoins);
        }
    }
}
```
