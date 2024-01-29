// This is for generating bitboard for specific board condition in https://fumen.zui.jp/ .
// Use it with F12 console while showing the https://fumen.zui.jp/ with your favorite board.
Array.from(Array.from(document.getElementsByTagName('table'))[24].getElementsByTagName('td'))
    .map(a => ["rgb(51, 51, 51)", "rgb(0, 0, 0)"].includes(a.style["background-color"]) ? 0 : 1)
    .join('').match(/.{1,10}/g).reverse().map(a => `0b111${a}111`).join()