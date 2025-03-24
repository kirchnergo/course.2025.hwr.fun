#time "on"
let answer = 5I **(int (4I ** (int (3I ** 2))));;
let sans = answer.ToString()
let l = sans.Length
let prefix = sans.Substring(0,20)
let suffix = sans.Substring(l-20)
#time "off"
printfn "Length = %d, digits %s ... %s" l prefix suffix