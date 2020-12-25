namespace SomeAcme.SomeUtilNamespace
{
    public class GenericMemoryCacheOptions
    {
    
        public GenericMemoryCacheOptions()
        {
            DefaultExpirationInSeconds = 0;
        }

        public string PrefixKey { get; set; }
        public int DefaultExpirationInSeconds { get; set; }
    }
}
