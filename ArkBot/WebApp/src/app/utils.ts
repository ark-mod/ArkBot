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