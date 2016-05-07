using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CPlayer : MonoBehaviour {
	
	public List<short> cell_indexes { get; private set; }
	public byte player_index { get; private set; }	
    
	// Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void initialize(int index)
	{
		
	}
	
	public void add(short cell)
	{
		if (this.cell_indexes.Contains(cell))
		{
			Debug.LogError(string.Format("Already have a cell. {0}", cell));
			return;
		}
		
		this.cell_indexes.Add(cell);
	}
	
	public void remove(short cell)
	{
		this.cell_indexes.Remove(cell);
	}
}
