#pragma once

class HResultException : public std::exception
{
private:
	HRESULT hr;
public:
	HResultException(HRESULT hr, const char* message)
		: std::exception(message)
	{
		this->hr = hr;
	}

	HRESULT HResult() const noexcept {
		return hr;
	}
};