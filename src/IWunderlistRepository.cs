using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blueclass.Wunderlist
{
    public interface IWunderlistRepository
    {
        Task<IEnumerable<WunderlistList>> CreateLists(IEnumerable<string> lists);
        Task<Tuple<bool, IEnumerable<WunderlistTask>>> Synchronise(IEnumerable<WunderlistTask> tasks);
        Task Synchronise(IEnumerable<WunderlistNote> notes);
        Task<WunderlistNote> GetNote(int id);
        Task Insert(WunderlistNote note);
        Task UpdateTasksOrder(int listId, IEnumerable<int> orderedTasksIds);
        Task<IEnumerable<WunderlistFile>> GetFiles(WunderlistTask wTask);
        Task Insert(WunderlistTask wTask, WunderlistFile attachment, byte[] content);
    }
}