using Avalonia.Threading;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTest.Base.Utils
{
    [System.AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class RunOnUIAttribute : Attribute, IWrapTestMethod
    {
        public TestCommand Wrap(TestCommand command) => new RunOnUICommand(command);

        class RunOnUICommand : DelegatingTestCommand
        {
            public RunOnUICommand(TestCommand innerCommand)
                : base(innerCommand)
            {
            }

            public override TestResult Execute(TestExecutionContext context)
            {
                var resultTask = Dispatcher.UIThread.InvokeAsync<object>(() => RunTest(context));

                resultTask.Wait();

                if (resultTask.Result is Exception ex)
                    throw ex;

                return (TestResult)resultTask.Result;
            }

            private object RunTest(TestExecutionContext context)
            {
                try
                {
                    return innerCommand.Execute(context);
                }
                catch (Exception e)
                {
                    return e;
                }
            }
        }
    }
}
