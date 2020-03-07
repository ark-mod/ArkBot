// ----------------------------------------------------
// Compare functions for sorting
// ----------------------------------------------------

export function floatCompare(v1 : number, v2 : number, asc: boolean, decimals?: number): number {
  let nullCheck = nullCompare(v1, v2, asc);
  if (nullCheck != undefined) return nullCheck;

  let base = Math.pow(10, decimals);
  let f1 = decimals != undefined ? Math.round(v1 * base) / base : v1;
  let f2 = decimals != undefined ? Math.round(v2 * base) / base : v2;
  return f1 > f2 ? (asc ? 1 : -1) : f1 < f2 ? (asc ? -1 : 1) : 0;
}

export function intCompare(v1 : number, v2 : number, asc: boolean): number {
  let nullCheck = nullCompare(v1, v2, asc);
  if (nullCheck != undefined) return nullCheck;

  return v1 > v2 ? (asc ? 1 : -1) : v1 < v2 ? (asc ? -1 : 1) : 0;
}

export function stringLocaleCompare(v1: string, v2: string, asc: boolean): number {
  let nullCheck = nullCompare(v1, v2, asc);
  if (nullCheck != undefined) return nullCheck;

  let r = v1.localeCompare(v2);
  return asc ? r : (r == 1 ? -1 : r == -1 ? 1 : 0);
}

export function nullCompare(v1: any, v2: any, asc: boolean): number {
  if (v1 == null && v2 == null) return 0;
  else if (v1 == null) return 1; //always below
  else if (v2 == null) return -1; //always below

  return undefined;
}

// ----------------------------------------------------
// Assorted functions
// ----------------------------------------------------
export function copyToClipboard(doc: any, str: string): void {
  const copy = doc.createElement('textarea');
  copy.style.position = 'fixed';
  copy.style.left = '0';
  copy.style.top = '0';
  copy.style.opacity = '0';
  copy.value = str;
  doc.body.appendChild(copy);
  copy.focus();
  copy.select();
  doc.execCommand('copy');
  doc.body.removeChild(copy);
}

export function fromHsvToRgb(h: number, s: number, v: number): any {
  var r: number, g: number, b: number, i: number, f: number, p: number, q: number, t: number;

  i = Math.floor(h * 6);
  f = h * 6 - i;
  p = v * (1 - s);
  q = v * (1 - f * s);
  t = v * (1 - (1 - f) * s);

  switch (i % 6) {
      case 0: r = v, g = t, b = p; break;
      case 1: r = q, g = v, b = p; break;
      case 2: r = p, g = v, b = t; break;
      case 3: r = p, g = q, b = v; break;
      case 4: r = t, g = p, b = v; break;
      case 5: r = v, g = p, b = q; break;
  }
  
  return {
      r: Math.round(r * 255),
      g: Math.round(g * 255),
      b: Math.round(b * 255)
  };
}