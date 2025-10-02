using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TaskGroupState
{
    Inactive,
    Running,
    Complete
}

[System.Serializable]
public class TaskGroup
{
    [SerializeField]
    public Task[] tasks;

    public IReadOnlyList<Task> Tasks => tasks;
    public Quest Owner { get; private set; }
    public bool IsAllTaskComplete => tasks.All(x => x.IsComplete);
    public bool IsComplete => State == TaskGroupState.Complete;
    public TaskGroupState State { get; private set; }

    public TaskGroup()
    {
        tasks = new Task[0];
    }

    public TaskGroup(TaskGroup copyTarget)
    {
        tasks = copyTarget.Tasks.Select(x => Object.Instantiate(x)).ToArray();
    }

    public void Setup(Quest owner)
    {
        Owner = owner;
        foreach (var task in tasks)
            task.Setup(owner);
    }

    public void Start()
    {
        State = TaskGroupState.Running;
        foreach (var task in tasks)
            task.Start();
    }

    public void End()
    {
        foreach (var task in tasks)
            task.End();
    }

    public void ReceiveReport(string category, object target, int successCount)
    {
        Debug.Log($"[TaskGroup] 리포팅 받음 - 카테고리: {category}, 타겟: {target}, 성공수: {successCount}");
        Debug.Log($"[TaskGroup] 태스크 수: {tasks.Length}");
        
        foreach (var task in tasks)
        {
            Debug.Log($"[TaskGroup] 태스크 체크 - 카테고리: {task.Category?.CodeName}, 타겟 매치: {task.IsTarget(category, target)}");
            if (task.IsTarget(category, target))
            {
                Debug.Log($"[TaskGroup] 태스크 매치됨! 진행도 업데이트: {task.CurrentSuccess} -> {task.CurrentSuccess + successCount}");
                task.ReceiveReport(successCount);
            }
        }
    }

    public void Complete()
    {
        if (IsComplete)
            return;

        State = TaskGroupState.Complete;

        foreach (var task in tasks)
        {
            if (!task.IsComplete)
                task.Complete();
        }
    }

    public Task FindTaskByTarget(object target) => tasks.FirstOrDefault(x => x.ContainsTarget(target));

    public Task FindTaskByTarget(TaskTarget target) => FindTaskByTarget(target.Value);

    public bool ContainsTarget(object target) => tasks.Any(x => x.ContainsTarget(target));

    public bool ContainsTarget(TaskTarget target) => ContainsTarget(target.Value);
}
