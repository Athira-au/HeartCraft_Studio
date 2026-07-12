using UnityEngine;
using System.Collections.Generic;

public class MeshUndoManager : MonoBehaviour
{
    public static MeshUndoManager Instance;

    private Stack<Mesh> undoStack = new Stack<Mesh>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Save mesh BEFORE modification
    public void SaveState(Mesh mesh)
    {
        if (mesh == null) return;

        Mesh copy = Instantiate(mesh);
        undoStack.Push(copy);
    }

    // Restore previous mesh
    public Mesh Undo()
    {
        if (undoStack.Count == 0)
            return null;

        return undoStack.Pop();
    }

    public bool HasUndo()
    {
        return undoStack.Count > 0;
    }

    public void Clear()
    {
        undoStack.Clear();
    }
}

