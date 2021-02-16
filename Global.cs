using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public class Global : Node
{
  private static Global Instance { get; set; }
  private readonly Dictionary<string, Row> rows = new Dictionary<string, Row>();

  public static IEnumerable<Row> Rows => Instance.rows.Values;
  //Instance.rows.Values

  public override void _Ready()
  {
    base._Ready();
    Instance = this;
  }

  public static void Register(Row row)
  {
    if (Instance.rows.ContainsKey(row.Description)) return;
    
    Instance.rows[row.Description] = row;
  }
}
