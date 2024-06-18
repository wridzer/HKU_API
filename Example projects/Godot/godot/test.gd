extends Node

# Declare the functions from the DLL
# Make sure to put the correct path to your DLL
const DLL_PATH = "res://HKU_SDK.dll"

# Declare the functions from the DLL
@export var get_users_func = null
@export var configure_project_func = null
@export var open_login_page_func = null
@export var start_polling_func = null
@export var cancel_polling_func = null
@export var logout_func = null
@export var get_user_func = null
@export var get_leaderboard_func = null
@export var get_leaderboards_for_project_func = null
@export var upload_leaderboard_score_func = null
@export var set_output_callback_func = null

func _ready():
	# Load the DLL
	var dll = GDNative.new()
	dll.set_library(load(DLL_PATH))
	if not dll.initialize():
		print("Failed to initialize the DLL.")
		return

	# Load the functions from the DLL
	get_users_func = dll.get_function("GetUsers")
	configure_project_func = dll.get_function("ConfigureProject")
	open_login_page_func = dll.get_function("OpenLoginPage")
	start_polling_func = dll.get_function("StartPolling")
	cancel_polling_func = dll.get_function("CancelPolling")
	logout_func = dll.get_function("Logout")
	get_user_func = dll.get_function("GetUser")
	get_leaderboard_func = dll.get_function("GetLeaderboard")
	get_leaderboards_for_project_func = dll.get_function("GetLeaderboardsForProject")
	upload_leaderboard_score_func = dll.get_function("UploadLeaderboardScore")
	set_output_callback_func = dll.get_function("SetOutputCallback")

	# Test the function calls
	if get_users_func:
		get_users_func(_get_users_callback, null)

	if configure_project_func:
		configure_project_func("project_ID", _configure_project_callback, null)

	if open_login_page_func:
		open_login_page_func()

	if start_polling_func:
		start_polling_func(_polling_callback, null)

	if logout_func:
		logout_func(_logout_callback, null)

	if get_user_func:
		get_user_func("user_ID", _get_user_callback, null)

	if get_leaderboard_func:
		var outArray = []
		get_leaderboard_func("leaderboard_ID", outArray, 10, 0, _leaderboard_callback, null)

	if get_leaderboards_for_project_func:
		var outArray = []
		get_leaderboards_for_project_func(outArray, _leaderboards_for_project_callback, null)

	if upload_leaderboard_score_func:
		upload_leaderboard_score_func("leaderboard_ID", 100, _upload_score_callback, null)

	if set_output_callback_func:
		set_output_callback_func(_output_callback, null)


# Callbacks for the DLL functions
func _get_users_callback(users, length, context):
	print("GetUsers Callback: ", users, length)

func _configure_project_callback(is_success, context):
	print("ConfigureProject Callback: ", is_success)

func _polling_callback(is_success, user_id, context):
	print("Polling Callback: ", is_success, user_id)

func _logout_callback(is_success, context):
	print("Logout Callback: ", is_success)

func _get_user_callback(username, length, context):
	print("GetUser Callback: ", username, length)

func _leaderboard_callback(is_success, context):
	print("Leaderboard Callback: ", is_success)

func _leaderboards_for_project_callback(is_success, context):
	print("LeaderboardsForProject Callback: ", is_success)

func _upload_score_callback(is_success, current_rank, context):
	print("UploadScore Callback: ", is_success, current_rank)

func _output_callback(message, context):
	print("Output Callback: ", message)
