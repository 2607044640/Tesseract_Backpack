using Godot;

[GlobalClass]
public partial class ItemPhysicsComponent : RigidBody2D
{
    private Control _rootControl;
    private GridShapeComponent _gridShapeComp;
    private System.Collections.Generic.List<CollisionShape2D> _dynamicShapes = new System.Collections.Generic.List<CollisionShape2D>();

    public bool IsPhysical { get; private set; } = false;

    public override void _Ready()
    {
        this.TopLevel = true; // Break the infinite translation feedback loop by detaching from parent transform
        _rootControl = GetParent<Control>();
        
        if (_rootControl != null)
        {
            _gridShapeComp = _rootControl.GetNodeOrNull<GridShapeComponent>("%GridShapeComponent");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsPhysical || _rootControl == null) return;

        // CRITICAL: Offset compensation (Origin is now Top-Left)
        _rootControl.GlobalPosition = this.GlobalPosition;
    }

    public void EnablePhysics()
    {
        if (_gridShapeComp != null)
        {
            // Clear existing dynamic shapes
            foreach (var shape in _dynamicShapes)
            {
                if (GodotObject.IsInstanceValid(shape))
                {
                    shape.QueueFree();
                }
            }
            _dynamicShapes.Clear();

            Vector2 cellSize = _gridShapeComp.CellSize;
            
            // Native Compound Shapes: Create a CollisionShape2D for each cell
            foreach (Vector2I cellPos in _gridShapeComp.CurrentLocalCells)
            {
                var shapeNode = new CollisionShape2D();
                var rectShape = new RectangleShape2D();
                rectShape.Size = cellSize;
                shapeNode.Shape = rectShape;
                
                // Position is the center of the cell relative to the top-left origin
                shapeNode.Position = new Vector2(cellPos.X * cellSize.X + cellSize.X / 2f, cellPos.Y * cellSize.Y + cellSize.Y / 2f);
                
                AddChild(shapeNode);
                _dynamicShapes.Add(shapeNode);
            }
        }

        // Sync position before unfreezing
        if (_rootControl != null)
        {
            this.GlobalPosition = _rootControl.GlobalPosition;
        }

        this.SetDeferred(RigidBody2D.PropertyName.Freeze, false);
        IsPhysical = true;
    }

    public void DisablePhysics()
    {
        this.SetDeferred(RigidBody2D.PropertyName.Freeze, true);
        // We don't need to disable shapes individually, freezing the RigidBody is enough.
        // Dynamic shapes will be recreated on next EnablePhysics.
        IsPhysical = false;

        LinearVelocity = Vector2.Zero;
        AngularVelocity = 0f;
        Position = Vector2.Zero;
    }
}
