using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using LoveTester.UI;

public class Global : Node
{
  private static Global Instance { get; set; }
  private readonly Dictionary<string, Row> rows = new Dictionary<string, Row>();

  public static IEnumerable<Row> Rows => Instance.rows.Values;
  
  public static readonly string MainButton = "main_button";
  public static readonly string MenuButton = "menu_button";

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
