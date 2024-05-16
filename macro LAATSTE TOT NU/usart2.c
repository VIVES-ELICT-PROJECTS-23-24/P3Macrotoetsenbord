#include "stm32f091xc.h"
#include "usart2.h"

#define MAX_STRING_LENGTH 3
char rxBuffer[MAX_STRING_LENGTH];

void InitUsart2(uint32_t baudRate)
{
	// Clock voor GPIOA inschakelen.
	RCC->AHBENR = RCC->AHBENR | RCC_AHBENR_GPIOAEN;
	
	// Pinnen van de USART2 instellen als alternate function.	
	// PA2 => TX-pin
	GPIOA->MODER = (GPIOA->MODER & ~GPIO_MODER_MODER2) | GPIO_MODER_MODER2_1;		// Alternate function op PA2
	GPIOA->AFR[0] |= 0x00000100;																								// USART2_TX is alternate function AF1 (zie datasheet STM32F091RC, p43).

	// PA3 => RX-pin
	GPIOA->MODER = (GPIOA->MODER & ~GPIO_MODER_MODER3) | GPIO_MODER_MODER3_1;		// Alternate function op PA3
	GPIOA->AFR[0] |= 0x00001000;
		
	// Usart module van een klok voorzien
	RCC->APB1ENR |= RCC_APB1ENR_USART2EN;
	
	// Baudrate zetten
	USART2->BRR = 48000000/baudRate;
	
	// Reception overrun disable. Als nieuwe byte aankomt terwijl
	// vorige nog niet verwerkt is, geen overrun error genereren.
	USART2->CR3 |= USART_CR3_OVRDIS;
	
	// USART inschakelen
	USART2->CR1 |= USART_CR1_UE;
	
	// Transmitter enable
	USART2->CR1 |= USART_CR1_TE;	
	
//	// Interrupt voor de receiver enable (receiver buffer not empty interrupt enable)
//	USART2->CR1 |= USART_CR1_RXNEIE;
//	
//	// Koppeling van interrupt maken met de NVIC
//	NVIC_SetPriority(USART2_IRQn, 0);
//	NVIC_EnableIRQ(USART2_IRQn);
//	
//	// Receiver enable
//	USART2->CR1 |= USART_CR1_RE;
}

void StringToUsart2(char* string)
{
	uint8_t indexer = 0;
	
	while(string[indexer] != 0)
	{
		// Byte versturen.
		USART2->TDR = (uint8_t)string[indexer++];
		
		// Wachten tot byte vertuurd is.
		while((USART2->ISR & USART_ISR_TC) != USART_ISR_TC);
	}
}

uint8_t receiveUsart2(void)
{
	uint8_t rxData = 0;
 
  rxData = USART2->RDR;
  
  return rxData;
}


char* TESTreceiveUsart2String(void)
{
    char rxChar;
    int index = 0;

    while (index < MAX_STRING_LENGTH - 1) { // Receive until buffer full
        // Wait for a character to be received from USART2
        while (!(USART2->ISR & USART_ISR_RXNE)); // Wait until data is ready to be read
        
        // Receive a character from USART2
        rxChar = USART2->RDR;

        // Store the received character in the buffer
        rxBuffer[index++] = rxChar;

            rxBuffer[index] = '\0'; // Terminate the string
        
    }
		return rxBuffer; // Return the received string
}


//char* receiveUsart2String(void)
//{
//    static char rxBuffer[MAX_STRING_LENGTH]; // Buffer to store received string
//    char rxChar;
//    int index = 0;

//    do {
//        // Receive a character from USART2
//        rxChar = USART2->RDR;
//        
//        // Store the received character in the buffer
//        rxBuffer[index++] = rxChar;
//    } while (rxChar != '\0' && index < MAX_STRING_LENGTH - 1); // Keep receiving until null terminator or buffer full
//    
//    // Add null terminator to mark the end of the string
//    rxBuffer[index] = '\0';

//    return rxBuffer;
//}

char receiveUsart2String(void)
{
    char rxChar;

    rxChar = USART2->RDR;

		return rxChar;
}

/*
// printf redirecting. Op die manier kan je 'printf("Hallo wereld");' gebruiken
// i.p.v. 'StringToUsart2("Hallo wereld");'.
//
// Vergeet USART 2 niet in te stellen (en InitUsart2() op te roepen).
struct __FILE
{
  int handle;
	// Whatever you require here. If the only file you are using is
	// standard output using printf() for debugging, no file handling
	// is required.
	//
	// https://community.arm.com/developer/tools-software/tools/f/keil-forum/34791/how-printf-to-specific-usart
};
// FILE is typedef’d in stdio.h.
FILE __stdout;

int fputc(int ch, FILE *f)
{
	// Your implementation of fputc().	
	
	// Byte versturen.
	USART2->TDR = ch;
	
	// Wachten tot byte vertuurd is.
	while((USART2->ISR & USART_ISR_TC) != USART_ISR_TC);	
  
  return ch;
}
*/
