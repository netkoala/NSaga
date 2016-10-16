﻿using System;
using System.Reflection;
using NSaga.Pipeline;
using TinyIoC;


namespace NSaga
{
    public static class Wireup
    {
        public static InternalMediatorBuilder UseInternalContainer()
        {
            var builder = new InternalMediatorBuilder(TinyIoCContainer.Current);

            return builder;
        }

        public static InternalMediatorBuilder UseInternalContainer(TinyIoCContainer container)
        {
            var builder = new InternalMediatorBuilder(container);

            return builder;
        }
    }


    public class InternalMediatorBuilder
    {
        private readonly TinyIoC.TinyIoCContainer container;
        private Assembly[] assembliesToScan;
        private readonly CompositePipelineHook compositePipeline;

        public InternalMediatorBuilder(TinyIoCContainer container)
        {
            this.container = container;
            this.compositePipeline = new CompositePipelineHook();
            RegisterDefaults();
        }

        private void RegisterDefaults()
        {
            UseServiceLocator<TinyIocServiceLocator>();
            UseMessageSerialiser<JsonNetSerialiser>();
            UseRepository<InMemorySagaRepository>();
            AddAssembliesToScan(AppDomain.CurrentDomain.GetAssemblies());

            compositePipeline.AddHook(new MetadataPipelineHook(container.Resolve<IMessageSerialiser>()));
            container.Register<IPipelineHook>(compositePipeline);

            container.Register<Assembly[]>((c, p) => AppDomain.CurrentDomain.GetAssemblies());

            container.Register<ISagaMediator, SagaMediator>();
        }

        public InternalMediatorBuilder UseMessageSerialiser<TSerialiser>() where TSerialiser : IMessageSerialiser
        {
            container.Register(typeof(IMessageSerialiser), typeof(TSerialiser));

            return this;
        }

        public InternalMediatorBuilder UseRepository<TRepository>() where TRepository : ISagaRepository
        {
            container.Register(typeof(ISagaRepository), typeof(TRepository));

            return this;
        }

        public InternalMediatorBuilder UseServiceLocator<TServiceLocator>() where TServiceLocator : IServiceLocator
        {
            container.Register(typeof(IServiceLocator), typeof(TServiceLocator));

            return this;
        }

        public InternalMediatorBuilder AddAssembliesToScan(Assembly[] assemblies)
        {
            assembliesToScan = assemblies;

            return this;
        }

        public InternalMediatorBuilder AddPiplineHook(IPipelineHook pipelineHook)
        {
            compositePipeline.AddHook(pipelineHook);

            return this;
        }

        public ISagaMediator Build()
        {
            var mediator = container.Resolve<ISagaMediator>();
            return mediator;
        }
    }
}