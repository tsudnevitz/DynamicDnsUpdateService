namespace DynamicDnsUpdateService.Dns
{
  public interface IDnsProviderFactory
  {
    IDnsProvider Create();
  }

  public class DnsProviderFactory : IDnsProviderFactory
  {
    public IDnsProvider Create()
    {
      return null;
    }
  }
}