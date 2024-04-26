extends Node3D


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	var s = HKU_SDK.new()
	s.add(10)
	s.add(20)
	s.add(30)
	print(s.get_total())
	s.reset()
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
