using System;

namespace CoinSheet3 {
	public class Venue {
		string name;
		string address;

		public string GetName() {
			return name;
		}

		public string GetAddress() {
			return address;
		}

		public void SetName(string s) {
			name = s;
		}

		public void SetAddress(string s) {
			address = s;
		}
	}
}