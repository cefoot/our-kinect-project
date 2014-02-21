package de.cefoot.data;

import java.util.HashMap;

public class Position extends HashMap<String, Long> {
	
	public Position(long x, long y, long z){
		setX(x);
		setY(y);
		setZ(z);
	}

	public long getX() {
		return get("x");
	}

	public long getY() {
		return get("y");
	}

	public long getZ() {
		return get("z");
	}

	public void setX(long value) {
		put("x", value);
	}

	public void setY(long value) {
		put("y", value);
	}

	public void setZ(long value) {
		put("z", value);
	}

}
