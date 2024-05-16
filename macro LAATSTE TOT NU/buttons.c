#include "stm32f091xc.h"
#include "stdbool.h"
#include "buttons.h"

// Knoppen als input initialiseren.
void InitButtons(void)
{
	// Clock voor GPIOA inschakelen.
	RCC->AHBENR = RCC->AHBENR | RCC_AHBENR_GPIOAEN;
	
	// Clock voor GPIOB inschakelen.
	RCC->AHBENR = RCC->AHBENR | RCC_AHBENR_GPIOBEN;
	
	// Clock voor GPIOC inschakelen.
	RCC->AHBENR = RCC->AHBENR | RCC_AHBENR_GPIOCEN;	
	
	// Alle pinnen met drukknoppen verbonden, op input zetten.
	GPIOA->MODER = GPIOA->MODER & ~GPIO_MODER_MODER1;
	GPIOA->MODER = GPIOA->MODER & ~GPIO_MODER_MODER4;	
	GPIOB->MODER = GPIOB->MODER & ~GPIO_MODER_MODER0;
	GPIOB->MODER = GPIOC->MODER & ~GPIO_MODER_MODER10;
	GPIOC->MODER = GPIOC->MODER & ~GPIO_MODER_MODER1;
  GPIOB->MODER = GPIOC->MODER & ~GPIO_MODER_MODER4;
	
	GPIOB->PUPDR |= GPIO_PUPDR_PUPDR4_0;
	GPIOB->PUPDR |= GPIO_PUPDR_PUPDR10_0;
}

bool SW1Active(void)
{
	if((GPIOA->IDR & GPIO_IDR_1) != GPIO_IDR_1)
			return true;
	else
			return false;
}		
	
bool SW2Active(void)
{
	if((GPIOA->IDR & GPIO_IDR_4) != GPIO_IDR_4)
			return true;
	else
			return false;
}

bool BovenActive(void)
{
	if((GPIOB->IDR & GPIO_IDR_0) != GPIO_IDR_0)
			return true;
	else
			return false;
}

bool OnderActive(void)
{
	if((GPIOC->IDR & GPIO_IDR_1) != GPIO_IDR_1)
			return true;
	else
			return false;
}

bool LinksActive(void)
{
	if((GPIOB->IDR & GPIO_IDR_10) != GPIO_IDR_10)
			return true;
	else
			return false;
}	

bool RechtsActive(void)
{
	if((GPIOB->IDR & GPIO_IDR_4) != GPIO_IDR_4)
			return true;
	else
			return false;
}		



