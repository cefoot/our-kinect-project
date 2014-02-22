package de.cefoot.kinect.drawer.data;

import java.util.HashMap;

public class Color extends HashMap<String, Double> {
	
	public static Color getRandomColor(){
		Color clr = new Color();
		clr.setRed(Math.random());
		clr.setGreen(Math.random());
		clr.setBlue(Math.random());
		return clr;
	}

	public double getBlue() {
		return get("b");
	}

	public double getGreen() {
		return get("g");
	}

	public double getRed() {
		return get("r");
	}

	public void setBlue(double value) {
		put("b", value);
	}

	public void setGreen(double value) {
		put("g", value);
	}

	public void setRed(double value) {
		put("r", value);
	}

}
