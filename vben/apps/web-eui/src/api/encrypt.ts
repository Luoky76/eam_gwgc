export default function encryptFront(text: string): string {
    // 对文本进行URI编码，然后将%XX格式转换为对应字符，最后进行base64编码
    const uriEncoded = encodeURIComponent(text);
    const decodedFromUri = uriEncoded.replace(
        /%([0-9A-F]{2})/g,
        (_match, hexCode) => String.fromCharCode(parseInt(hexCode, 16))
    );
    let encodedText = btoa(decodedFromUri);

    const encodedLength = encodedText.length;
    
    if (encodedLength > 1) {
        // 生成随机位置（0到长度-2之间）
        const randomPos = Math.floor(Math.random() * (encodedLength - 1));
        // 计算中间索引位置
        const midIndex = Math.floor(encodedLength / 2);
        
        // 提取字符串各部分
        const latterPart = encodedText.slice(midIndex);
        const randomChar = encodedText.slice(randomPos, randomPos + 1);
        const formerPart = encodedText.slice(0, midIndex);
        
        // 重组字符串并进行反转和前后缀处理
        encodedText = latterPart + randomChar + formerPart;
        encodedText = `pqz${encodedText.split('').reverse().join('')}zpq`;
    }
    
    return encodedText;
};
