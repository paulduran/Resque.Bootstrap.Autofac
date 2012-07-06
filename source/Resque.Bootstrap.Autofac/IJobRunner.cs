namespace Resque.Bootstrap.Autofac
{
    public interface IJobRunner
    {
        void Process(object obj);
    }

    public interface IJobRunner<T> : IJobRunner
    {
        void Process(T job);
    }

    public abstract class BaseJobRuner<T> : IJobRunner<T>
    {
        public abstract void Process(T job);
        
        public void Process(object obj)
        {
            Process((T)obj);
        }
    }
}