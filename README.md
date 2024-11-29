# Bug Report: Exception During TakeMapOffline Task Cancellation

## Description

When performing a `TakeMapOffline` task and canceling it at **15%**, subtasks continue to run. While the cancellation appears to be handled correctly, switching views triggers **unobserved exceptions**, leading to crashes. 

### Observed Behavior

The following exception occurs when switching views after canceling the task:
```
A Task's exception(s) were not observed either by Waiting on the Task or accessing its Exception property. As a result, the unobserved exception was rethrown by the finalizer thread. (User canceled: Job canceled.)
System.AggregateException
Stack (Reflected by ExceptionHandler):
   at GI.Common.Core.AExceptionHandler.LogException(Exception e, String headerText, ExceptionOutputType exceptionOutputType)
   at GI.Common.Core.AExceptionHandler.HandleExceptionPreDoWork(Exception& e, ExceptionOutputType exceptionOutputType)
   at GI.Common.Core.AExceptionHandler.HandleException(Exception e, ExceptionOutputType exceptionOutputType)
   at GI.Common.Core.ExceptionHelper.HandleUnhandledException(Exception e)
   at GI.Common.Core.EnvironmentFactory.UnhandledExceptionHandlerOnUnhandledException(Object sender, UnhandledExceptionEventArgs e)
   at GI.Common.Core.UnhandledExceptionHandler.FireUnhandledException(Object sender, UnhandledExceptionEventArgs args)
   at GI.Common.Core.UnhandledExceptionHandler.FireUnhandledException(Object sender, Exception exception, Boolean isTerminating)
   at GI.Common.Core.UnhandledExceptionHandler.<>c.<Initialize>b__4_0(Object sender, UnobservedTaskExceptionEventArgs args)
   at System.Threading.Tasks.TaskScheduler.PublishUnobservedTaskException(Object sender, UnobservedTaskExceptionEventArgs ueea)
   at System.Threading.Tasks.TaskExceptionHolder.Finalize()


InnerException:
User canceled: Job canceled.
System.Threading.Tasks.TaskCanceledException
Stack (Reflected by ExceptionHandler):
   at GI.Common.Core.AExceptionHandler.ExtractExceptionMessageComplete(Exception e, ExceptionOutputType outputType, Boolean isInnerException, String& msg)
   at GI.Common.Core.AExceptionHandler.LogException(Exception e, String headerText, ExceptionOutputType exceptionOutputType)
   at GI.Common.Core.AExceptionHandler.HandleExceptionPreDoWork(Exception& e, ExceptionOutputType exceptionOutputType)
   at GI.Common.Core.AExceptionHandler.HandleException(Exception e, ExceptionOutputType exceptionOutputType)
   at GI.Common.Core.ExceptionHelper.HandleUnhandledException(Exception e)
   at GI.Common.Core.EnvironmentFactory.UnhandledExceptionHandlerOnUnhandledException(Object sender, UnhandledExceptionEventArgs e)
   at GI.Common.Core.UnhandledExceptionHandler.FireUnhandledException(Object sender, UnhandledExceptionEventArgs args)
   at GI.Common.Core.UnhandledExceptionHandler.FireUnhandledException(Object sender, Exception exception, Boolean isTerminating)
   at GI.Common.Core.UnhandledExceptionHandler.<>c.<Initialize>b__4_0(Object sender, UnobservedTaskExceptionEventArgs args)
   at System.Threading.Tasks.TaskScheduler.PublishUnobservedTaskException(Object sender, UnobservedTaskExceptionEventArgs ueea)
   at System.Threading.Tasks.TaskExceptionHolder.Finalize()

```
### Reason: 
https://stackoverflow.com/questions/3284137/taskscheduler-unobservedtaskexception-event-handler-never-being-triggered/3284286#3284286
The UnobservedTaskException will happen if a Task gets collected by the GC with an exception unobserved. 
Assumption: Esri doesn’t handle the Subtasks correctly

Trigger for UnobservedTaskException: changing the view, then GC gets triggered
