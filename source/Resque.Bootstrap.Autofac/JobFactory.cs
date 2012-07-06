using System;
using Autofac;
using Newtonsoft.Json;

namespace Resque.Bootstrap.Autofac
{
    public class JobFactory : IJobCreator
    {
        private readonly IContainer container;

        public JobFactory(IContainer container)
        {
            this.container = container;
        }

        public IJob CreateJob(IFailureService failureService, Worker worker, QueuedItem deserializedObject, string queue)
        {
            return new Job(this, failureService, worker, deserializedObject, queue);
        }

        public IJobRunner CreateRunner(object obj)
        {
            var handlerType = typeof (IJobRunner<>).MakeGenericType(obj.GetType());
            return (IJobRunner) container.Resolve(handlerType);
        }

        public class Job : IJob
        {
            private JobFactory Creator { get; set; }
            public IFailureService FailureService { get; private set; }
            public QueuedItem Payload { get; private set; }
            public string Queue { get; private set; }
            public IWorker Worker { get; private set; }

            public Job(JobFactory creator, IFailureService failureService, IWorker worker, QueuedItem item,
                       string queue)
            {
                Creator = creator;
                FailureService = failureService;
                Worker = worker;
                Payload = item;
                Queue = queue;
            }

            public void Perform()
            {
                var jobData = DeserializeJobData(Payload);
                var runner = Creator.CreateRunner(jobData);
                runner.Process(jobData);
            }

            private object DeserializeJobData(QueuedItem payload)
            {
                string typeName = payload.@class;
                var type = Type.GetType(typeName);
                var jobData = JsonConvert.DeserializeObject(payload.args[0], type);
                return jobData;
            }

            public void Failed(Exception exception)
            {
                FailureService.Create(Payload, exception, Worker, Queue);
            }
        }
    }
}