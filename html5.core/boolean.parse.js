if (!Boolean.parse) {
	Boolean.parse = function (val) {
		return new RegExp("^true$", "i").test(val);
	};
}