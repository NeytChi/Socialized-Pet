﻿using Domain.GettingSubscribes;

namespace Infrastructure
{
    public class TaskGsRepository : ITaskGsRepository, ITaskGettingSubscribesRepository
    {
        private Context _context;

        public TaskGsRepository(Context context) 
        {
            _context = context;
        }
        public void Create(TaskGS taskGS)
        {
            _context.TaskGS.Add(taskGS);
            _context.SaveChanges();
        }
        public void Update(TaskGS taskGS)
        {
            _context.TaskGS.Update(taskGS);
            _context.SaveChanges();
        }
        public void Update(ICollection<TaskGS> tasks)
        {
            _context.TaskGS.UpdateRange(tasks);
            _context.SaveChanges();
        }
        public ICollection<TaskGS> GetBy(long accountId)
        {
            return _context.TaskGS.Where(t => t.AccountId == accountId).ToList();
        }
        public TaskGS GetBy(long taskId, bool taskDeleted = false)
        {
            return _context.TaskGS.Where(t => t.Id == taskId && !t.IsDeleted).FirstOrDefault();
        }
        public TaskGS GetBy(string userToken, long taskId, bool taskDeleted = false)
        {
             var task = _context.TaskGS
                .Join(_context.IGAccounts,
                      t => t.AccountId,
                      s => s.Id,
                      (t, s) => new { t, s })
                .Join(_context.Users,
                      ts => ts.s.UserId,
                      u => u.Id,
                      (ts, u) => new { ts.t, u })
                .Where(tu => tu.u.TokenForUse == userToken
                             && tu.t.Id == taskId
                             && tu.t.IsDeleted == taskDeleted)
                .Select(tu => tu.t)
                .FirstOrDefault();
            return task;
        }
    }
}
