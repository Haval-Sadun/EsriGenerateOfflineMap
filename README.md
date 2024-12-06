# Bug Report: No Exception handling of Subtasks from GenerateOfflineMapJob 

## Description
When performing the OfflineMapTask.GenerateOfflineMap job (see MainPageViewModel.TakeMapOffline()) sub tasks are created and not handled correctly. The same is probably true for syncronizing offline maps.
If an error occurs in a subtask or the job is cancelled UnobservedTaskExceptions occur.
The UnobservedTaskException will happen if a Task gets collected by the GC with an exception unobserved. Reason for that is https://stackoverflow.com/questions/3284137/taskscheduler-unobservedtaskexception-event-handler-never-being-triggered/3284286#3284286

### Reproduce the Behaviour
In the example the job is cancelled at **15%**, we catch the TaskCanceledException of the 'main' job. SubTask exceptions are not handled. This happens when the Button "TakeMapOffline" is clicked the second time (first time doesn't raise the excpeitons for some reasons)

### Observed Behaviour
The following exception occurs when switching views after canceling the task:
```
[DOTNET] Unobserved Exception:
[DOTNET] Exception Type: System.AggregateException
[DOTNET] Message: A Task's exception(s) were not observed either by Waiting on the Task or accessing its Exception property. As a result, the unobserved exception was rethrown by the finalizer thread. (User canceled: Job canceled.)
[DOTNET] Stack Trace: 
[DOTNET] This is an AggregateException with 1 inner exceptions:
[DOTNET] 	Exception Type: System.Threading.Tasks.TaskCanceledException
[DOTNET] 	Message: User canceled: Job canceled.
[DOTNET] 	Stack Trace: 
[DOTNET] 	Inner Exception:
[DOTNET] 		Exception Type: Esri.ArcGISRuntime.ArcGISRuntimeException
[DOTNET] 		Message: User canceled: Job canceled.
[DOTNET] 		Stack Trace: 
```

### Assumption: 
GenerateOfflineMapJob and OfflineMapSyncJob jobs don't handle the Subtask errors.



