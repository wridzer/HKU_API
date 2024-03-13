#pragma once

namespace HKU_API {

	typedef void(*HKU_CALLBACK)(char** users);

	class HKU {
	public:
		static void GetUsers(HKU_CALLBACK callback, void* context);
	};
}