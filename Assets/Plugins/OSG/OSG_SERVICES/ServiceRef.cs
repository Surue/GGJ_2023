// Old Skull Games
// Bernard Barthelemy
// Friday, November 8, 2019

namespace OSG.Services
{
    /// <summary>
    /// Convenient way to lazily access a service.
    /// </summary>
    /// 
    public class ServiceRef<I> where I : class
    {
        private I _value;
        public I Value => _value ?? (_value = ServiceProvider.GetService<I>());
    }

    public class ServiceRef<I,T> where T : class, I where I : class
    {
        private T _value;
        public T Value => _value ?? (_value = ServiceProvider.GetService<I>() as T);
    }
}
