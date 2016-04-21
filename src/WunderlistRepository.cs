using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Blueclass.Diagnostics;

namespace Blueclass.Wunderlist
{
    public class WunderlistRepository : IWunderlistRepository
    {
        readonly IWunderlistConnector _wunderlistConnector;
        readonly ILogger _logger;

        const string ListsRequest = "lists";
        const string NotesRequest = "notes";
        const string TasksRequest = "tasks";
        const string PositionsRequest = "task_positions";
        const string FilesRequest = "files";
        const string UploadsRequest = "uploads";

        public WunderlistRepository(IWunderlistConnector wunderlistConnector, ILogger logger)
        {
            _wunderlistConnector = wunderlistConnector;
            _logger = logger;
        }

        public async Task<IEnumerable<WunderlistList>> CreateLists(IEnumerable<string> lists)
        {
            var existingLists = await _wunderlistConnector.Get<WunderlistList[]>(ListsRequest);

            var newLists = lists.Except(existingLists.Select(list => list.Name));

            var wunderlistResponses = await Task.WhenAll(newLists.Select(list => _wunderlistConnector.Post(ListsRequest, new WunderlistList { Name = list })));

            var wLists = wunderlistResponses.Select(response => response.ResponseObject).ToArray();

            return wLists.Union(existingLists);
        }

        public async Task<Tuple<bool, IEnumerable<WunderlistTask>>> Synchronise(IEnumerable<WunderlistTask> tasks)
        {
            var groupings = from task in tasks
                            group task by task.ListId
                                into grouping
                            select grouping;

            var parallelTasks = groupings.Select(grouping => this.ProcessCategory(grouping.Key, grouping));

            var groupProcessed = (await Task.WhenAll(parallelTasks)).ToArray();

            return new Tuple<bool, IEnumerable<WunderlistTask>>(groupProcessed.Any(b => !b.Item1), groupProcessed.SelectMany(tuple => tuple.Item2));
        }

        public async Task Synchronise(IEnumerable<WunderlistNote> notes)
        {
            foreach (var note in notes)
                await _wunderlistConnector.Patch($"{NotesRequest}/{note.Id}", new { revision = note.Revision, content = note.Content });
        }

        public async Task<WunderlistNote> GetNote(int id)
        {
            return (await _wunderlistConnector.Get<IEnumerable<WunderlistNote>>($"{NotesRequest}?task_id={id}")).FirstOrDefault();
        }

        public async Task Insert(WunderlistNote note) => await _wunderlistConnector.Post(NotesRequest, note);

        public async Task UpdateTasksOrder(int listId, IEnumerable<int> orderedTasksIds)
        {
            var positions = (await _wunderlistConnector.Get<IEnumerable<ListPositions>>($"{PositionsRequest}?list_id={listId}")).First();

            positions.Order = orderedTasksIds.ToArray();

            await _wunderlistConnector.Patch($"{PositionsRequest}/{listId}", positions);
        }

        public async Task<IEnumerable<WunderlistFile>> GetFiles(WunderlistTask wTask)
        {
            return await _wunderlistConnector.Get<IEnumerable<WunderlistFile>>($"{FilesRequest}?task_id={wTask.Id}");
        }
        
        public async Task Insert(WunderlistTask wTask, WunderlistFile attachment, byte[] content)
        {
            var request = new WunderlistUpload
            {
                ContentType = MimeMapping.GetMimeMapping(attachment.FileName), //"application/octet-stream",
                FileName = attachment.FileName,
                FileSize = content.Length,
                //Md5Sum = _hashGenerator.GetMd5(content)
            };

            var result = await _wunderlistConnector.Post<WunderlistUpload, WunderlistUploadResponse>(UploadsRequest, request);

            var amazonResult = await _wunderlistConnector.PostUpload(result.ResponseObject.Part.Url, content, result.ResponseObject.Part.Authorization, result.ResponseObject.Part.Date);

            if (!amazonResult)
                _logger.Log(LogLevel.Error, $"Amazon upload of task '{wTask.Title}' attachment failed for file: {attachment.FileName}.");

            // Do we want to mark it as finished also when it's failed?
            await _wunderlistConnector.Patch(UploadsRequest + $"/{result.ResponseObject.Id}", new { state = "finished" } );

            await _wunderlistConnector.Post(FilesRequest, new { upload_id = result.ResponseObject.Id, task_id = wTask.Id } );
        }

        async Task<Tuple<bool, IEnumerable<WunderlistTask>>> ProcessCategory(int listId, IEnumerable<WunderlistTask> tasks)
        {
            tasks = tasks.ToArray();

            var tasksList = (await _wunderlistConnector.Get<IEnumerable<WunderlistList>>(ListsRequest)).First(list => list.Id == listId);

            var wTasks = await _wunderlistConnector.Get<WunderlistTask[]>($"{TasksRequest}?list_id={tasksList.Id}");

            // If extra on Wunderlist, mark it as done.
            var deletedRemoteTasks = from remoteTask in wTasks
                                     where !remoteTask.Completed && !tasks.Select(task => task.Title).Contains(remoteTask.Title)
                                     select remoteTask;

            foreach (var deletedRemoteTask in deletedRemoteTasks)
            {
                deletedRemoteTask.Completed = true;

                await _wunderlistConnector.Patch($"{TasksRequest}/{deletedRemoteTask.Id}", deletedRemoteTask);
            }

            // TODO: Sync attachments.

            // If on both, sync the data.
            //var commonTasks = from task in tasks
            //                  join wTask in wTasks on task.Title equals wTask.Title
            //                  select SynchroniseTask(task, wTask);

            //if (commonTasks.Any(b => false))
            //    _logger.Log(LogLevel.Warning, "Error updating tasks.");

            // If extra here, add to Wunderlist.
            var missingTasks = from task in tasks
                               where wTasks.All(wunderlistTask => wunderlistTask.Title != task.Title)
                               select new WunderlistTask { Title = task.Title, ListId = tasksList.Id };

            var successStatus = true;
            var returnTasks = new List<WunderlistTask>();

            // Amazingly, if a task has been marked as completed will appear in missing tasks
            // so we will recreate it, perfect!
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var missingTask in missingTasks)
            {
                var wunderlistResponse = await _wunderlistConnector.Post(TasksRequest, missingTask);

                successStatus = successStatus && wunderlistResponse.IsSuccessStatusCode;

                if (wunderlistResponse.IsSuccessStatusCode)
                    returnTasks.Add(wunderlistResponse.ResponseObject);
            }

            return new Tuple<bool, IEnumerable<WunderlistTask>>(successStatus, returnTasks.Union(wTasks));
        }

        //static bool SynchroniseTask(WunderlistTask master, WunderlistTask slave) => true;
    }
}