namespace diep;
using Godot;
using System;

public partial class GameManager : Node
{
    private int _playerXP = 0;

    public void _on_Target_Destroyed(int xp)
    {
        _playerXP += xp;
        UpdateLevel();
    }

    private void UpdateLevel()
    {
        // Implement level-up logic here
    }
}
